using DesktopNode.Contracts;

namespace DesktopNode.Api;

internal sealed class DesktopNodeApiAuthSessionHandler
{
    private readonly DesktopNodeAccountAuthService accountAuth;

    public DesktopNodeApiAuthSessionHandler(DesktopNodeAccountAuthOptions? options)
    {
        accountAuth = new DesktopNodeAccountAuthService(options);
    }

    public DesktopNodeApiResponse? TryHandle(
        DesktopNodeApiRequest request,
        string method,
        string path)
    {
        if (!DesktopNodeApiRuntimeRoutes.IsAccountAuthRoute(method, path))
        {
            return null;
        }

        if (method == "POST" && path == "/api/v1/auth/login")
        {
            var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "auth.login");
            return !parsed.Ok ? parsed.Response! : AuthResult(accountAuth.Login(parsed.Value!.Value));
        }

        if (method == "POST" && path == "/api/v1/auth/loopback-session")
        {
            return AuthResult(accountAuth.CreateLoopbackSession(request.RemoteIsLoopback));
        }

        if (method == "POST" && path == "/api/v1/auth/refresh")
        {
            var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "auth.refresh");
            return !parsed.Ok ? parsed.Response! : AuthResult(accountAuth.Refresh(parsed.Value!.Value, request.RemoteIsLoopback));
        }

        if (method == "POST" && path == "/api/v1/auth/logout")
        {
            var parsed = string.IsNullOrWhiteSpace(request.Body)
                ? DesktopNodeApiRequestParsing.ParsedJson.Success(DesktopNodeApiResponseFactory.EmptyObject())
                : DesktopNodeApiRequestParsing.TryParseBody(request.Body, "auth.logout");
            return !parsed.Ok ? parsed.Response! : AuthResult(accountAuth.Logout(parsed.Value!.Value));
        }

        if (method == "GET" && path == "/api/v1/auth/session")
        {
            var validation = accountAuth.ValidateSessionAccessToken(request.Authorization);
            if (!validation.Ok)
            {
                return AuthValidationFailure("auth.session", validation);
            }

            return DesktopNodeApiResponseFactory.Json(
                200,
                DesktopNodeApiResponseFactory.Body(true, "auth.session", accountAuth.BuildSessionData(validation.Principal!), null));
        }

        if (method == "GET" && path == "/api/v1/auth/rbac")
        {
            var validation = accountAuth.ValidateSessionAccessToken(request.Authorization);
            if (!validation.Ok)
            {
                return AuthValidationFailure("auth.rbac", validation);
            }

            return DesktopNodeApiResponseFactory.Json(
                200,
                DesktopNodeApiResponseFactory.Body(true, "auth.rbac", accountAuth.BuildRbacData(), null));
        }

        return DesktopNodeApiResponseFactory.Failure(
            404,
            "api.route",
            "PCV_ROUTE_NOT_FOUND",
            $"No auth route matches '{path}'.",
            "The requested auth route is not part of the Desktop Node API contract.",
            false);
    }

    public DesktopNodeApiResponse? Authorize(
        DesktopNodeApiRequest request,
        string method,
        string path)
    {
        if (!accountAuth.Ready)
        {
            return null;
        }

        var requiredPermission = RequiredPermissionForRoute(method, path);
        if (requiredPermission is null)
        {
            return null;
        }

        var validation = accountAuth.ValidateAccessToken(request.Authorization);
        if (!validation.Ok)
        {
            return AuthValidationFailure("api.auth", validation);
        }

        if (accountAuth.HasPermission(validation.Principal!, requiredPermission))
        {
            return null;
        }

        if (DesktopNodeApiRuntimeRoutes.TryMatchContract(method, path, out var routeMatch) &&
            string.Equals(routeMatch.Route.RouteFamily, "guest-execution", StringComparison.Ordinal))
        {
            return DesktopNodeApiResponseFactory.Failure(
                403,
                GuestExecutionOperationFor(routeMatch.Route.OperationName),
                GuestExecutionProblemCodes.PermissionDenied,
                "The current account role is not allowed to use guest execution routes.",
                $"Required guest execution permission: {requiredPermission}. Current role: {validation.Principal!.Role}.",
                false,
                "Grant the explicit ADR-0009 guest execution capability before opening this route.");
        }

        return DesktopNodeApiResponseFactory.Failure(
            403,
            "api.rbac",
            "PCV_RBAC_FORBIDDEN",
            "The current account role is not allowed to use this route.",
            $"Required permission: {requiredPermission}. Current role: {validation.Principal!.Role}.",
            false);
    }

    public string ResolveActor(DesktopNodeApiRequest request)
    {
        if (accountAuth.Ready)
        {
            var validation = accountAuth.ValidateAccessToken(request.Authorization);
            if (validation.Ok && validation.Principal is not null)
            {
                return validation.Principal.Username;
            }
        }

        return string.IsNullOrWhiteSpace(request.ClientIdentity)
            ? "local-api-operator"
            : request.ClientIdentity!;
    }

    public RuntimePolicyAuthPolicy CreateRuntimePolicy(string tokenStorage)
    {
        return accountAuth.CreateRuntimePolicy(tokenStorage);
    }

    private static string? RequiredPermissionForRoute(string method, string path)
    {
        if (DesktopNodeApiRuntimeRoutes.TryMatchContract(method, path, out var routeMatch))
        {
            return routeMatch.Route.RequiredPermission;
        }

        return "read";
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

    // 아래 두 개만 이 소유자에 남는다. 정규 helper 에 대응물이 없는 auth 고유 wrapper 이고,
    // 이름도 DesktopNodeApiResponseFactory 와 겹치지 않으므로 사본이 아니다.
    private static DesktopNodeApiResponse AuthResult(DesktopNodeAuthActionResult result)
    {
        return DesktopNodeApiResponseFactory.Json(
            result.StatusCode,
            DesktopNodeApiResponseFactory.Body(result.Ok, result.Operation, result.Data, result.Error));
    }

    private static DesktopNodeApiResponse AuthValidationFailure(
        string operation,
        DesktopNodeAuthValidationResult validation)
    {
        return DesktopNodeApiResponseFactory.Json(
            validation.StatusCode,
            DesktopNodeApiResponseFactory.Body(false, operation, null, validation.Error));
    }
}
