using System.Text.RegularExpressions;
using DesktopNode.Contracts;

namespace DesktopNode.Api;

// ADR-0009 가 고정한 guest 실행 경계. preview 두 개와 차단 응답이
// 같은 계약을 공유하므로 한 소유자가 갖는다.
internal sealed class DesktopNodeApiGuestExecutionRouteHandler
{
    private readonly DesktopNodeApiAuthSessionHandler authSessionHandler;

    public DesktopNodeApiGuestExecutionRouteHandler(DesktopNodeApiAuthSessionHandler authSessionHandler)
    {
        this.authSessionHandler = authSessionHandler;
    }

    public DesktopNodeApiResponse? TryHandle(DesktopNodeApiRequest request, string method, string normalizedPath)
    {
        if (method == "POST" && DesktopNodeApiRequestParsing.TryMatch(normalizedPath, "^/api/v1/vms/([^/]*)/guest/exec/preview$", out var guestExecPreviewMatch))
        {
            return HandleGuestExecPreviewRoute(request, guestExecPreviewMatch);
        }

        if (method == "POST" && DesktopNodeApiRequestParsing.TryMatch(normalizedPath, "^/api/v1/vms/([^/]*)/guest/channel/preview$", out var guestChannelPreviewMatch))
        {
            return HandleGuestChannelPreviewRoute(request, guestChannelPreviewMatch);
        }

        return HandleGuestExecutionBoundaryRoute(method, normalizedPath);
    }

    private DesktopNodeApiResponse HandleGuestExecPreviewRoute(
        DesktopNodeApiRequest request,
        Match guestExecPreviewMatch)
    {
        const string operation = "vm.guest.exec.preview";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(guestExecPreviewMatch.Groups[1].Value, operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var command = DesktopNodeApiJsonReader.ReadStringList(parsed.Value!.Value, "command");
        if (command.Count == 0)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.CommandRequired,
                "Guest execution preview requires a command array.",
                "Pass command as a non-empty JSON string array; execution remains disabled until ADR-0009 gates pass.",
                false);
        }

        var credentialRef = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "credential_ref");
        var credential = GuestExecutionCredentialReferenceResolver.Resolve(credentialRef);
        var credentialSupplied = !string.IsNullOrWhiteSpace(credentialRef);
        if (credentialSupplied && !credential.Ok)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.CredentialRefRequired,
                "Guest execution accepts only protected credential references.",
                "Use wincred:<target>, credential-manager:<target>, or dpapi:<target>; do not pass raw secrets.",
                false);
        }

        var environment = DesktopNodeApiJsonReader.ReadStringDictionary(parsed.Value.Value, "environment");
        var timeoutSeconds = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "timeout_sec") ?? 60;
        if (timeoutSeconds is < 1 or > 600)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.Timeout,
                "Guest execution preview timeout is outside the supported range.",
                "Pass timeout_sec between 1 and 600 seconds.",
                false);
        }

        var redaction = GuestExecutionRedactor.Redact(command, environment);
        if (redaction.RedactionApplied)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.SecretRedactionRequired,
                "Guest execution command contains secret-like material.",
                "Move secrets into a protected credential reference before previewing or queueing guest execution.",
                false);
        }

        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation,
            request.RequestId!,
            authSessionHandler.ResolveActor(request),
            routeId.Value!,
            credentialRef,
            redaction,
            "preview_only_execute_available");
        var result = new GuestExecutionPreviewResult(
            GuestExecutionSecurityContract.GuestExecPreviewContract,
            "dry-run",
            routeId.Value!,
            PreviewEnabled: true,
            ExecuteEnabled: true,
            ExecutionQueued: false,
            HostMutationPerformed: false,
            RequiredCapability: "guest.exec",
            CredentialReference: new GuestExecutionCredentialReferenceProjection(
                credentialSupplied,
                credential.Ok,
                credential.Storage,
                credential.Target is null ? null : GuestExecutionContractHasher.Hash(credential.Target),
                credential.ErrorCode),
            CommandHash: audit.CommandHash,
            Redaction: redaction,
            AuditPreview: audit,
            TimeoutSeconds: timeoutSeconds,
            NextAction: "Dry-run validates credential, redaction, audit, and timeout shape without queueing a job; omit --dry-run to queue guest execution.");

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, operation, result, null));
    }

    private DesktopNodeApiResponse HandleGuestChannelPreviewRoute(
        DesktopNodeApiRequest request,
        Match guestChannelPreviewMatch)
    {
        const string operation = "vm.guest.channel.preview";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(guestChannelPreviewMatch.Groups[1].Value, operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var redaction = GuestExecutionRedactor.Redact(["guest-agent-ensure-channel", "--dry-run"], new Dictionary<string, string>());
        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation,
            request.RequestId!,
            authSessionHandler.ResolveActor(request),
            routeId.Value!,
            credentialRef: null,
            redaction,
            "preview_only_verify_repair_available");
        var result = new GuestChannelPreviewResult(
            GuestExecutionSecurityContract.GuestChannelPreviewContract,
            "dry-run",
            routeId.Value!,
            PreviewEnabled: true,
            VerifyEnabled: true,
            RepairEnabled: true,
            HostMutationPerformed: false,
            GuestCommandPerformed: false,
            RequiredCapability: "guest.channel.configure",
            CandidateTransports: ["windows-powershell-direct"],
            AuditPreview: audit,
            NextAction: "Dry-run validates channel audit shape without host mutation; use --verify or --repair --yes to queue provider work.");

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, operation, result, null));
    }

    private static DesktopNodeApiResponse? HandleGuestExecutionBoundaryRoute(string method, string path)
    {
        if (!string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase) ||
            !DesktopNodeApiRuntimeRoutes.TryMatch(method, path, out var routeMatch) ||
            !string.Equals(routeMatch.Route.RouteFamily, "guest-execution", StringComparison.Ordinal))
        {
            return null;
        }

        return DesktopNodeApiResponseFactory.Failure(
            403,
            GuestExecutionOperationFor(routeMatch.Route.OperationName),
            GuestExecutionProblemCodes.Disabled,
            "Guest execution and guest channel mutation are disabled by runtime policy.",
            "ADR-0009 keeps this route closed until credential reference, audit, redaction, timeout/cancel, and RBAC evidence pass.",
            false,
            "Follow ADR-0009 and run the guest-execution-policy-api-preview-package-fullgate-manual-admin gate before enabling execution.");
    }

    private static string GuestExecutionOperationFor(string routeOperationName)
    {
        return routeOperationName switch
        {
            "PreviewVmGuestExec" => "vm.guest.exec.preview",
            "QueueVmGuestExec" => "vm.guest.exec",
            "PreviewVmGuestChannel" => "vm.guest.channel.preview",
            "QueueVerifyVmGuestChannel" or "VerifyVmGuestChannel" => "vm.guest.channel.verify",
            "QueueEnsureVmGuestChannel" or "EnsureVmGuestChannel" => "vm.guest.channel.ensure",
            _ => "vm.guest.execution"
        };
    }
}
