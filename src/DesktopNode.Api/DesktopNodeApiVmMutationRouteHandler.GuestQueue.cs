using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Contracts;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiVmMutationRouteHandler
{
    private DesktopNodeApiResponse HandleGuestFilePreviewRoute(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        var parsed = TryReadGuestFileRequest(
            request,
            routeMatch.Parameters["vmId"],
            "vm.guest.file.preview",
            out var parameters);
        if (parsed is not null)
        {
            return parsed;
        }

        return DesktopNodeApiResponseFactory.OperationResponse(operationInvoker.Invoke(
            "vm.guest.file.preview",
            DesktopNodeApiResponseFactory.JsonFromObject(parameters),
            cancellationToken));
    }

    private DesktopNodeApiResponse QueueVmGuestFile(DesktopNodeApiRequest request, DesktopNodeApiRouteMatch routeMatch)
    {
        var parsed = TryReadGuestFileRequest(
            request,
            routeMatch.Parameters["vmId"],
            "vm.guest.file",
            out var parameters);
        if (parsed is not null)
        {
            return parsed;
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            "vm.guest.file",
            DesktopNodeApiResponseFactory.JsonFromObject(parameters),
            request.RequestId!));
    }

    private static DesktopNodeApiResponse? TryReadGuestFileRequest(
        DesktopNodeApiRequest request,
        string encodedVmId,
        string operation,
        out SortedDictionary<string, object?> parameters)
    {
        parameters = [];
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(encodedVmId, operation);
        if (!routeId.Ok)
        {
            return routeId.Response;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response;
        }

        var hostPath = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "host_path");
        var guestPath = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "guest_path");
        var credentialRef = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "credential_ref");
        var direction = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "direction") ?? GuestFileJobContract.DirectionHostToGuest;
        var sharedFolder = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "shared_folder");
        var timeoutSeconds = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "timeout_sec") ?? 60;
        var sizeBytes = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "size_bytes");
        if (timeoutSeconds is < 1 or > 600)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.Timeout,
                "Guest file timeout is outside the supported range.",
                "Pass timeout_sec between 1 and 600 seconds.",
                false);
        }

        var evaluation = GuestFileJobContract.Evaluate(new GuestFileJobRequest(
            direction,
            hostPath,
            guestPath,
            sizeBytes ?? 1,
            credentialRef,
            sharedFolder));
        if (!evaluation.Ok)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                evaluation.ErrorCode ?? GuestFileJobProblemCodes.PathNotAllowed,
                "Guest file job request is outside the allowlist.",
                "Use host-to-guest, credential-ref, and allowlisted paths/size. HGFS shared folders are forbidden.",
                false);
        }

        parameters = new SortedDictionary<string, object?>
        {
            ["name"] = routeId.Value,
            ["credential_ref"] = credentialRef,
            ["direction"] = evaluation.Direction,
            ["guest_path"] = evaluation.NormalizedGuestPath,
            ["host_path"] = evaluation.NormalizedHostPath,
            ["timeout_sec"] = timeoutSeconds
        };
        if (sizeBytes is not null)
        {
            parameters["size_bytes"] = sizeBytes.Value;
        }

        return null;
    }

    private DesktopNodeApiResponse QueueVmGuestExec(DesktopNodeApiRequest request, DesktopNodeApiRouteMatch routeMatch)
    {
        const string operation = "vm.guest.exec";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], operation);
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
                "Guest execution requires a command array.",
                "Pass command as a non-empty JSON string array.",
                false);
        }

        var credentialRef = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "credential_ref");
        var credential = GuestExecutionCredentialReferenceResolver.Resolve(credentialRef);
        if (string.IsNullOrWhiteSpace(credentialRef) || !credential.Ok)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.CredentialRefRequired,
                "Guest execution requires a protected credential reference.",
                "Use wincred:<target>, credential-manager:<target>, or dpapi:<path>; do not pass raw secrets.",
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
                "Guest execution timeout is outside the supported range.",
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
                "Move secrets into a protected credential reference before queueing guest execution.",
                false);
        }

        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation,
            request.RequestId!,
            authSessionHandler.ResolveActor(request),
            routeId.Value!,
            credentialRef,
            redaction,
            "queued");
        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["actor"] = authSessionHandler.ResolveActor(request),
                ["audit_preview"] = audit,
                ["command"] = command,
                ["credential_ref"] = credentialRef,
                ["environment"] = environment,
                ["name"] = routeId.Value,
                ["request_id"] = request.RequestId!,
                ["timeout_sec"] = timeoutSeconds
            }),
            request.RequestId!));
    }

    private DesktopNodeApiResponse QueueVmGuestChannelVerify(DesktopNodeApiRequest request, DesktopNodeApiRouteMatch routeMatch)
    {
        const string operation = "vm.guest.channel.verify";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var credentialRef = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "credential_ref");
        var credential = GuestExecutionCredentialReferenceResolver.Resolve(credentialRef);
        if (string.IsNullOrWhiteSpace(credentialRef) || !credential.Ok)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.CredentialRefRequired,
                "Guest channel verification requires a protected credential reference.",
                "Use wincred:<target>, credential-manager:<target>, or dpapi:<path>; do not pass raw secrets.",
                false);
        }

        var timeoutSeconds = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "timeout_sec") ?? 60;
        if (timeoutSeconds is < 1 or > 600)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.Timeout,
                "Guest channel verification timeout is outside the supported range.",
                "Pass timeout_sec between 1 and 600 seconds.",
                false);
        }

        var redaction = GuestExecutionRedactor.Redact(["guest-agent-ensure-channel", "--verify"], new Dictionary<string, string>());
        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation,
            request.RequestId!,
            authSessionHandler.ResolveActor(request),
            routeId.Value!,
            credentialRef,
            redaction,
            "queued");
        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["actor"] = authSessionHandler.ResolveActor(request),
                ["audit_preview"] = audit,
                ["credential_ref"] = credentialRef,
                ["mode"] = "verify",
                ["name"] = routeId.Value,
                ["request_id"] = request.RequestId!,
                ["timeout_sec"] = timeoutSeconds
            }),
            request.RequestId!));
    }

    private DesktopNodeApiResponse QueueVmGuestChannelEnsure(DesktopNodeApiRequest request, DesktopNodeApiRouteMatch routeMatch)
    {
        const string operation = "vm.guest.channel.ensure";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        if (!DesktopNodeApiJsonReader.ReadBool(parsed.Value!.Value, "yes"))
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                "PCV_GUEST_CHANNEL_REPAIR_CONFIRMATION_REQUIRED",
                "Guest channel repair requires explicit confirmation.",
                "Pass yes=true or use pcvcli vm guest-agent-ensure-channel <vm> --repair --yes.",
                false);
        }

        var redaction = GuestExecutionRedactor.Redact(["guest-agent-ensure-channel", "--repair", "--yes"], new Dictionary<string, string>());
        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation,
            request.RequestId!,
            authSessionHandler.ResolveActor(request),
            routeId.Value!,
            credentialRef: null,
            redaction,
            "queued");
        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["actor"] = authSessionHandler.ResolveActor(request),
                ["audit_preview"] = audit,
                ["mode"] = "repair",
                ["name"] = routeId.Value,
                ["request_id"] = request.RequestId!,
                ["timeout_sec"] = 60,
                ["yes"] = true
            }),
            request.RequestId!));
    }
}
