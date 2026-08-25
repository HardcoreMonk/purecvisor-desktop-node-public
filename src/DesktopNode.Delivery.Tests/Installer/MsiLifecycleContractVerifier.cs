using System.Text.RegularExpressions;

namespace DesktopNode.Delivery.Tests.Installer;

internal sealed record MsiLifecycleStepProjection(
    string Name,
    string Phase,
    string FilePath,
    IReadOnlyList<string> Arguments,
    IReadOnlyList<int> SuccessExitCodes,
    IReadOnlyList<int> ConditionalExitCodes,
    bool MutatesHost);

internal sealed record MsiLifecycleNoAutoRebootProjection(
    bool Enabled,
    string RestartManagerControl,
    string RebootProperty,
    string NoRestartArgument,
    int RebootInitiatedExitCode);

internal sealed record MsiLifecyclePlanProjection(
    int SchemaVersion,
    string MsiPath,
    string LogDirectory,
    MsiLifecycleNoAutoRebootProjection NoAutoReboot,
    IReadOnlyList<MsiLifecycleStepProjection> Steps);

internal sealed record MsiLifecycleExitProjection(
    bool Ok,
    string Phase,
    int ExitCode,
    string Result,
    bool RebootRequired,
    bool ActualRebootInitiated,
    bool RequiresPostRebootVerification);

internal static partial class MsiLifecycleContractVerifier
{
    private const string ErrorCode = "PCV_INSTALLER_MSI_LIFECYCLE_SOURCE_INVALID";

    internal static MsiLifecyclePlanProjection BuildPlan(
        string source,
        string msiPath,
        string logDirectory)
    {
        if (string.IsNullOrWhiteSpace(msiPath))
        {
            throw Invalid("msi-path");
        }

        if (string.IsNullOrWhiteSpace(logDirectory))
        {
            throw Invalid("log-directory");
        }

        var stepBody = ExtractFunction(source, "New-PcvMsiLifecycleStep");
        RequireContains(
            stepBody,
            "$arguments=@($MsiArguments)+@('REBOOT=ReallySuppress','MSIRESTARTMANAGERCONTROL=Disable','/qn','/norestart','/l*vx',$LogPath)",
            "common-arguments");
        RequireContains(stepBody, "file_path='msiexec.exe'", "file-path");
        RequireContains(stepBody, "success_exit_codes=@($SuccessExitCodes)", "success-exit-codes");
        RequireContains(stepBody, "conditional_exit_codes=@($ConditionalExitCodes)", "conditional-exit-codes");
        RequireContains(stepBody, "mutates_host=$true", "mutates-host");

        var smokeBody = ExtractFunction(source, "New-PcvMsiLifecycleSmokePlan");
        RequireStep(
            smokeBody,
            "New-PcvMsiLifecycleStep-Name'install'-PhaseInstall-MsiArguments@('/i',$fullMsiPath)-LogPath(Join-Path$fullLogDirectory'install.log')",
            "install");
        RequireStep(
            smokeBody,
            "New-PcvMsiLifecycleStep-Name'repair'-PhaseRepair-MsiArguments@('/i',$fullMsiPath,'REINSTALL=ALL','REINSTALLMODE=vomus')-LogPath(Join-Path$fullLogDirectory'repair.log')-SuccessExitCodes@(0)-ConditionalExitCodes@(3010)",
            "repair");
        RequireStep(
            smokeBody,
            "New-PcvMsiLifecycleStep-Name'uninstall-preserve'-PhaseUninstall-MsiArguments@('/x',$fullMsiPath)-LogPath(Join-Path$fullLogDirectory'uninstall-preserve.log')",
            "uninstall-preserve");
        RequireStep(
            smokeBody,
            "New-PcvMsiLifecycleStep-Name'install-remove-data'-PhaseInstallRemoveData-MsiArguments@('/i',$fullMsiPath)-LogPath(Join-Path$fullLogDirectory'install-remove-data.log')",
            "install-remove-data");
        RequireStep(
            smokeBody,
            "New-PcvMsiLifecycleStep-Name'uninstall-remove-data'-PhaseUninstallRemoveData-MsiArguments@('/x',$fullMsiPath,'REMOVE_DATA=1')-LogPath(Join-Path$fullLogDirectory'uninstall-remove-data.log')",
            "uninstall-remove-data");
        RequireContains(smokeBody, "schema_version=1", "schema-version");
        RequireContains(
            smokeBody,
            "no_auto_reboot=[pscustomobject][ordered]@{enabled=$truereboot_property='ReallySuppress'restart_manager_control='Disable'norestart_argument='/norestart'reboot_initiated_exit_code=1641}",
            "no-auto-reboot");

        var fullMsiPath = Path.GetFullPath(msiPath);
        var fullLogDirectory = Path.GetFullPath(logDirectory);
        var common = new[]
        {
            "REBOOT=ReallySuppress",
            "MSIRESTARTMANAGERCONTROL=Disable",
            "/qn",
            "/norestart",
            "/l*vx",
        };

        MsiLifecycleStepProjection Step(
            string name,
            string phase,
            IReadOnlyList<string> msiArguments,
            string logName,
            IReadOnlyList<int>? success = null,
            IReadOnlyList<int>? conditional = null) =>
            new(
                name,
                phase,
                "msiexec.exe",
                [.. msiArguments, .. common, Path.Combine(fullLogDirectory, logName)],
                success ?? [0],
                conditional ?? [],
                true);

        return new MsiLifecyclePlanProjection(
            1,
            fullMsiPath,
            fullLogDirectory,
            new MsiLifecycleNoAutoRebootProjection(true, "Disable", "ReallySuppress", "/norestart", 1641),
            [
                Step("install", "Install", ["/i", fullMsiPath], "install.log"),
                Step(
                    "repair",
                    "Repair",
                    ["/i", fullMsiPath, "REINSTALL=ALL", "REINSTALLMODE=vomus"],
                    "repair.log",
                    [0],
                    [3010]),
                Step("uninstall-preserve", "Uninstall", ["/x", fullMsiPath], "uninstall-preserve.log"),
                Step("install-remove-data", "InstallRemoveData", ["/i", fullMsiPath], "install-remove-data.log"),
                Step(
                    "uninstall-remove-data",
                    "UninstallRemoveData",
                    ["/x", fullMsiPath, "REMOVE_DATA=1"],
                    "uninstall-remove-data.log"),
            ]);
    }

    internal static MsiLifecycleExitProjection Classify(
        string source,
        string phase,
        int exitCode,
        bool assertionsPassed)
    {
        if (phase is not ("Install" or "Repair" or "Uninstall" or "InstallRemoveData" or "UninstallRemoveData"))
        {
            throw Invalid("phase");
        }

        var body = ExtractFunction(source, "ConvertTo-PcvMsiLifecycleExitClassification");
        RequireContains(
            body,
            "if($ExitCode-eq0){return[pscustomobject][ordered]@{ok=$truephase=$Phaseexit_code=$ExitCoderesult='success'reboot_required=$falseactual_reboot_initiated=$falserequires_post_reboot_verification=$false}}",
            "classification:zero");
        RequireContains(
            body,
            "if($ExitCode-eq1641){return[pscustomobject][ordered]@{ok=$falsephase=$Phaseexit_code=$ExitCoderesult='reboot_initiated_failure'reboot_required=$trueactual_reboot_initiated=$truerequires_post_reboot_verification=$true}}",
            "classification:1641");
        RequireContains(
            body,
            "if($ExitCode-eq3010-and$Phase-eq'Repair'){return[pscustomobject][ordered]@{ok=[bool]$AssertionsPassedphase=$Phaseexit_code=$ExitCoderesult=if($AssertionsPassed){'reboot_required_success'}else{'reboot_required_pending_assertions'}reboot_required=$trueactual_reboot_initiated=$falserequires_post_reboot_verification=$true}}",
            "classification:repair-3010");
        RequireContains(
            body,
            "result='unexpected_exit_code'reboot_required=$ExitCode-eq3010actual_reboot_initiated=$falserequires_post_reboot_verification=$ExitCode-eq3010",
            "classification:unexpected");

        if (exitCode == 0)
        {
            return new MsiLifecycleExitProjection(true, phase, exitCode, "success", false, false, false);
        }

        if (exitCode == 1641)
        {
            return new MsiLifecycleExitProjection(
                false,
                phase,
                exitCode,
                "reboot_initiated_failure",
                true,
                true,
                true);
        }

        if (exitCode == 3010 && phase == "Repair")
        {
            return new MsiLifecycleExitProjection(
                assertionsPassed,
                phase,
                exitCode,
                assertionsPassed ? "reboot_required_success" : "reboot_required_pending_assertions",
                true,
                false,
                true);
        }

        var rebootRequired = exitCode == 3010;
        return new MsiLifecycleExitProjection(
            false,
            phase,
            exitCode,
            "unexpected_exit_code",
            rebootRequired,
            false,
            rebootRequired);
    }

    private static void RequireStep(string source, string expected, string detail) =>
        RequireContains(source, expected, $"step:{detail}");

    private static void RequireContains(string source, string expected, string detail)
    {
        if (!Canonical(source).Contains(Canonical(expected), StringComparison.Ordinal))
        {
            throw Invalid(detail);
        }
    }

    private static string ExtractFunction(string source, string name)
    {
        var match = Regex.Match(
            source,
            $@"(?m)^\s*function\s+{Regex.Escape(name)}\s*\{{",
            RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            throw Invalid($"function:{name}");
        }

        var open = source.IndexOf('{', match.Index);
        var depth = 0;
        var quote = '\0';
        var lineComment = false;
        for (var index = open; index < source.Length; index++)
        {
            var current = source[index];
            var next = index + 1 < source.Length ? source[index + 1] : '\0';
            if (lineComment)
            {
                if (current == '\n')
                {
                    lineComment = false;
                }
                continue;
            }

            if (quote != '\0')
            {
                if (quote == '\'' && current == '\'' && next == '\'')
                {
                    index++;
                }
                else if (quote == '"' && current == '`')
                {
                    index++;
                }
                else if (current == quote)
                {
                    quote = '\0';
                }
                continue;
            }

            if (current == '#')
            {
                lineComment = true;
            }
            else if (current is '\'' or '"')
            {
                quote = current;
            }
            else if (current == '{')
            {
                depth++;
            }
            else if (current == '}' && --depth == 0)
            {
                return source[(open + 1)..index];
            }
        }

        throw Invalid($"function-brace:{name}");
    }

    private static string Canonical(string value) =>
        WhitespaceRegex().Replace(
            value.Replace("`\r\n", string.Empty, StringComparison.Ordinal)
                .Replace("`\n", string.Empty, StringComparison.Ordinal),
            string.Empty);

    private static InvalidDataException Invalid(string detail) =>
        new($"{ErrorCode}|{detail}");

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
