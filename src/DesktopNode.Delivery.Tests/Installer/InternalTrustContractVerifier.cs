using System.Text;
using System.Text.RegularExpressions;

namespace DesktopNode.Delivery.Tests.Installer;

internal sealed record InternalTrustDryRunProjection(
    int ExitCode,
    bool Ok,
    bool DryRun,
    string SigningStoreScope,
    string TrustStoreScope,
    bool LocalMachineAdminRequired,
    bool AdminGateEvaluated,
    string SigningTrustModel,
    bool SecretsRecorded);

internal sealed record InternalTrustCertificateBoundary(
    IReadOnlyList<string> PublicCertificateCommands,
    IReadOnlyList<string> PublicCertificateFileNames,
    string TrustedPublisherStore,
    string SigningMode,
    string SigningTrustModel,
    string CertificateReference,
    bool ExportsPrivateKey,
    bool RecordsPfxPassword);

internal sealed record InternalTrustDocumentationBoundary(
    string RunbookHeading,
    bool HasDryRunExample,
    bool StatesNoLocalMachineDryRunImport,
    bool StatesRequireSignedMode,
    bool StatesInternalEnterpriseModel,
    bool StatesAdministratorOptIn,
    bool StatesSecretBoundary,
    bool StatesNonPublicationBoundary);

internal static partial class InternalTrustContractVerifier
{
    private const string ErrorCode = "PCV_INSTALLER_INTERNAL_TRUST_SOURCE_INVALID";
    private const string AdminExpression =
        "($SigningStoreScope-eq'LocalMachine'-or($TrustStoreScope-eq'LocalMachine'-and-not[bool]$SkipTrustInstall))";

    internal static InternalTrustDryRunProjection ProjectDryRun(
        string source,
        string signingStoreScope,
        string trustStoreScope,
        bool skipTrustInstall = false)
    {
        RequireScope(signingStoreScope, nameof(signingStoreScope));
        RequireScope(trustStoreScope, nameof(trustStoreScope));

        var planBody = ExtractNamedFunction(source, "New-PcvInternalTrustPlan");
        var plan = ParseAssignments(planBody);
        RequireExpression(plan, "signing_store_scope", "$SigningStoreScope");
        RequireExpression(plan, "trust_store_scope", "$TrustStoreScope");
        RequireExpression(plan, "local_machine_admin_required", AdminExpression);
        RequireExpression(plan, "signing_trust_model", "'InternalEnterprise'");
        RequireExpression(plan, "secrets_recorded", "$false");

        var dryRunIndex = source.IndexOf("if ($DryRun)", StringComparison.Ordinal);
        var adminGateIndex = source.IndexOf(
            "if ($plan.local_machine_admin_required -and -not [bool]$plan.administrator)",
            StringComparison.Ordinal);
        if (dryRunIndex < 0 || adminGateIndex < 0 || dryRunIndex >= adminGateIndex)
        {
            throw Invalid("dry-run-before-admin-gate");
        }

        var dryRunBody = ExtractBraceBlock(source, source.IndexOf('{', dryRunIndex));
        var dryRun = ParseAssignments(dryRunBody);
        RequireExpression(dryRun, "ok", "$true");
        RequireExpression(dryRun, "dry_run", "$true");
        RequireExpression(dryRun, "plan", "$plan");
        if (!Canonical(dryRunBody).Contains("Write-PcvJsonAndExit-ExitCode0-Payload([ordered]@{", StringComparison.Ordinal))
        {
            throw Invalid("dry-run-exit");
        }

        var adminRequired = signingStoreScope == "LocalMachine" ||
            (trustStoreScope == "LocalMachine" && !skipTrustInstall);
        return new InternalTrustDryRunProjection(
            0,
            true,
            true,
            signingStoreScope,
            trustStoreScope,
            adminRequired,
            false,
            "InternalEnterprise",
            false);
    }

    internal static InternalTrustCertificateBoundary InspectCertificateBoundary(string source)
    {
        var exportBody = ExtractNamedFunction(source, "Export-PcvPublicCertificate");
        RequireCommand(exportBody, "Export-Certificate");
        RequireCanonicalContains(exportBody, "Export-Certificate-Cert$Certificate-FilePath$path-Force", "export-arguments");

        var importBody = ExtractNamedFunction(source, "Import-PcvPublicCertificateToStore");
        RequireCommand(importBody, "Import-Certificate");
        RequireCanonicalContains(importBody, "Import-Certificate-FilePath$certPath-CertStoreLocation$storePath", "import-arguments");

        RequireCanonicalContains(
            source,
            "Import-PcvPublicCertificateToStore-Certificate$leaf-Scope$TrustStoreScope-StoreName'TrustedPublisher'",
            "trusted-publisher");
        RequireCanonicalContains(
            source,
            "-FileName'PureCVisor-Internal-CodeSigning-Root.cer'",
            "root-public-certificate");
        RequireCanonicalContains(
            source,
            "-FileName'PureCVisor-DesktopNode-Internal-CodeSigning.cer'",
            "leaf-public-certificate");

        var buildMarkerIndex = source.IndexOf("build_arguments = [ordered]@{", StringComparison.Ordinal);
        if (buildMarkerIndex < 0)
        {
            throw Invalid("build-arguments");
        }

        var buildBody = ExtractBraceBlock(source, source.IndexOf('{', buildMarkerIndex));
        var buildArguments = ParseAssignments(buildBody);
        RequireExpression(buildArguments, "SigningMode", "'RequireSigned'");
        RequireExpression(buildArguments, "SigningTrustModel", "'InternalEnterprise'");
        RequireExpression(buildArguments, "CertificateThumbprint", "$leaf.Thumbprint");

        var exportsPrivateKey = ForbiddenCommandRegex("Export-PfxCertificate").IsMatch(source);
        var recordsPfxPassword = PfxPasswordRegex().IsMatch(source);
        if (exportsPrivateKey)
        {
            throw Invalid("private-key-export");
        }

        if (recordsPfxPassword)
        {
            throw Invalid("pfx-password");
        }

        return new InternalTrustCertificateBoundary(
            ["Export-Certificate", "Import-Certificate"],
            ["PureCVisor-Internal-CodeSigning-Root.cer", "PureCVisor-DesktopNode-Internal-CodeSigning.cer"],
            "TrustedPublisher",
            "RequireSigned",
            "InternalEnterprise",
            "CertificateThumbprint",
            exportsPrivateKey,
            recordsPfxPassword);
    }

    internal static InternalTrustDocumentationBoundary InspectDocumentation(
        string installerReadme,
        string signingPolicyAdr,
        string verificationPolicy)
    {
        var markdown = MarkdownProjection.Parse(installerReadme);
        var heading = markdown.Headings.SingleOrDefault(value =>
            value.Equals("Internal RequireSigned gate runbook", StringComparison.Ordinal));
        if (heading is null)
        {
            throw Invalid("runbook-heading");
        }

        var hasDryRunExample = markdown.CodeBlocks.Any(block =>
            block.Contains("New-PcvInternalCodeSigningTrust.ps1", StringComparison.Ordinal) &&
            block.Contains("-DryRun", StringComparison.Ordinal));
        var combined = string.Join('\n', installerReadme, signingPolicyAdr, verificationPolicy);
        var result = new InternalTrustDocumentationBoundary(
            heading,
            hasDryRunExample,
            installerReadme.Contains("Dry-run은 LocalMachine trust import를 실행하지 않는다", StringComparison.Ordinal),
            installerReadme.Contains("SigningMode RequireSigned", StringComparison.Ordinal),
            installerReadme.Contains("SigningTrustModel InternalEnterprise", StringComparison.Ordinal),
            combined.Contains("관리자 opt-in", StringComparison.Ordinal),
            combined.Contains("private key/PFX/password", StringComparison.Ordinal),
            combined.Contains("public trusted signing 또는 외부 stable publication", StringComparison.Ordinal));

        if (!result.HasDryRunExample ||
            !result.StatesNoLocalMachineDryRunImport ||
            !result.StatesRequireSignedMode ||
            !result.StatesInternalEnterpriseModel ||
            !result.StatesAdministratorOptIn ||
            !result.StatesSecretBoundary ||
            !result.StatesNonPublicationBoundary)
        {
            throw Invalid("documentation-boundary");
        }

        return result;
    }

    private static IReadOnlyDictionary<string, string> ParseAssignments(string source)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match match in AssignmentRegex().Matches(source))
        {
            if (!result.TryAdd(match.Groups["key"].Value, match.Groups["value"].Value.Trim()))
            {
                throw Invalid($"duplicate-assignment:{match.Groups["key"].Value}");
            }
        }

        return result;
    }

    private static void RequireExpression(
        IReadOnlyDictionary<string, string> assignments,
        string key,
        string expected)
    {
        if (!assignments.TryGetValue(key, out var actual) ||
            !Canonical(actual).Equals(Canonical(expected), StringComparison.Ordinal))
        {
            throw Invalid($"assignment:{key}");
        }
    }

    private static void RequireCommand(string source, string command)
    {
        if (!CommandRegex(command).IsMatch(source))
        {
            throw Invalid($"command:{command}");
        }
    }

    private static void RequireCanonicalContains(string source, string expected, string detail)
    {
        if (!Canonical(source).Contains(Canonical(expected), StringComparison.Ordinal))
        {
            throw Invalid(detail);
        }
    }

    private static void RequireScope(string value, string detail)
    {
        if (value is not ("CurrentUser" or "LocalMachine"))
        {
            throw Invalid(detail);
        }
    }

    private static string ExtractNamedFunction(string source, string name)
    {
        var match = Regex.Match(
            source,
            $@"(?m)^\s*function\s+{Regex.Escape(name)}\s*\{{",
            RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            throw Invalid($"function:{name}");
        }

        return ExtractBraceBlock(source, source.IndexOf('{', match.Index));
    }

    private static string ExtractBraceBlock(string source, int openBraceIndex)
    {
        if (openBraceIndex < 0 || openBraceIndex >= source.Length || source[openBraceIndex] != '{')
        {
            throw Invalid("open-brace");
        }

        var depth = 0;
        var state = LexicalState.Normal;
        for (var index = openBraceIndex; index < source.Length; index++)
        {
            var current = source[index];
            var next = index + 1 < source.Length ? source[index + 1] : '\0';
            switch (state)
            {
                case LexicalState.LineComment:
                    if (current == '\n')
                    {
                        state = LexicalState.Normal;
                    }
                    continue;
                case LexicalState.BlockComment:
                    if (current == '#' && next == '>')
                    {
                        state = LexicalState.Normal;
                        index++;
                    }
                    continue;
                case LexicalState.SingleQuoted:
                    if (current == '\'' && next == '\'')
                    {
                        index++;
                    }
                    else if (current == '\'')
                    {
                        state = LexicalState.Normal;
                    }
                    continue;
                case LexicalState.DoubleQuoted:
                    if (current == '`')
                    {
                        index++;
                    }
                    else if (current == '"')
                    {
                        state = LexicalState.Normal;
                    }
                    continue;
            }

            if (current == '#')
            {
                state = LexicalState.LineComment;
            }
            else if (current == '<' && next == '#')
            {
                state = LexicalState.BlockComment;
                index++;
            }
            else if (current == '\'')
            {
                state = LexicalState.SingleQuoted;
            }
            else if (current == '"')
            {
                state = LexicalState.DoubleQuoted;
            }
            else if (current == '{')
            {
                depth++;
            }
            else if (current == '}' && --depth == 0)
            {
                return source[(openBraceIndex + 1)..index];
            }
        }

        throw Invalid("unmatched-brace");
    }

    private static string Canonical(string value) =>
        WhitespaceRegex().Replace(value, string.Empty);

    private static InvalidDataException Invalid(string detail) =>
        new($"{ErrorCode}|{detail}");

    [GeneratedRegex(@"(?m)^\s*(?<key>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<value>[^\r\n]+?)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex AssignmentRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"PFX\s+password", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PfxPasswordRegex();

    private static Regex CommandRegex(string command) =>
        new($@"(?m)^\s*{Regex.Escape(command)}(?:\s|`|$)", RegexOptions.CultureInvariant);

    private static Regex ForbiddenCommandRegex(string command) =>
        new($@"\b{Regex.Escape(command)}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private enum LexicalState
    {
        Normal,
        LineComment,
        BlockComment,
        SingleQuoted,
        DoubleQuoted,
    }

    private sealed record MarkdownProjection(
        IReadOnlyList<string> Headings,
        IReadOnlyList<string> CodeBlocks)
    {
        internal static MarkdownProjection Parse(string source)
        {
            var headings = new List<string>();
            var codeBlocks = new List<string>();
            StringBuilder? currentBlock = null;
            foreach (var line in source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
            {
                if (line.StartsWith("```", StringComparison.Ordinal))
                {
                    if (currentBlock is null)
                    {
                        currentBlock = new StringBuilder();
                    }
                    else
                    {
                        codeBlocks.Add(currentBlock.ToString());
                        currentBlock = null;
                    }
                    continue;
                }

                if (currentBlock is not null)
                {
                    currentBlock.AppendLine(line);
                    continue;
                }

                var heading = Regex.Match(line, @"^#{1,6}\s+(?<heading>.+?)\s*$", RegexOptions.CultureInvariant);
                if (heading.Success)
                {
                    headings.Add(heading.Groups["heading"].Value);
                }
            }

            if (currentBlock is not null)
            {
                throw Invalid("markdown-fence");
            }

            return new MarkdownProjection(headings, codeBlocks);
        }
    }
}
