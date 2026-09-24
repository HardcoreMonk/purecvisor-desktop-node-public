using DesktopNode.Contracts;
using DesktopNode.Host;
using DesktopNode.HyperV;

namespace DesktopNode.Host.Ops;

internal static class DesktopNodeHyperVSwitchOps
{
    public const string OperationFamily = "hyperv-switch";

    public static bool Owns(string? operation)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(operation, OperationFamily);
    }

    public static DesktopNodeHostServiceActionResult Execute(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeHyperVSwitchController switchController)
    {
        var action = options.ServiceAction ?? string.Empty;
        var switchName = options.SwitchName;
        var snapshot = TryQuery(switchController, switchName);
        var evaluation = Evaluate(action, options, snapshot);
        if (!evaluation.Ok)
        {
            return Failure(
                options,
                plan,
                snapshot,
                evaluation.ErrorCode ?? NetworkChangeProblemCodes.Forbidden,
                "The Hyper-V switch service action was rejected.");
        }

        if (options.DryRun)
        {
            return Success(options, plan, snapshot);
        }

        try
        {
            if (string.Equals(action, NetworkChangePolicy.ActionSwitchCreate, StringComparison.OrdinalIgnoreCase))
            {
                snapshot = switchController.Create(
                    evaluation.SwitchName!,
                    evaluation.SwitchType ?? NetworkChangePolicy.TypeInternal,
                    evaluation.AllowManagementOs);
            }
            else if (string.Equals(action, NetworkChangePolicy.ActionSwitchRemove, StringComparison.OrdinalIgnoreCase))
            {
                snapshot = switchController.Remove(evaluation.SwitchName!);
            }
            else
            {
                return Failure(
                    options,
                    plan,
                    snapshot,
                    "PCV_HOST_SERVICE_ACTION_INVALID",
                    $"Hyper-V switch action '{action}' is not supported.");
            }

            return Success(options, plan, snapshot);
        }
        catch (DesktopNodeHyperVNativeOperationException error)
        {
            return Failure(options, plan, snapshot, error.Code, error.Message);
        }
    }

    private static NetworkChangeEvaluation Evaluate(
        string action,
        DesktopNodeHostOptions options,
        DesktopNodeHyperVSwitchMutationSnapshot? snapshot)
    {
        var auth = new NetworkChangeAuthContext(HasAdmin: true);
        var exists = snapshot?.Exists == true;
        var attached = snapshot?.AttachedVmCount ?? 0;
        var switchType = options.SwitchType;
        var allowManagementOs = options.AllowManagementOs ??
            string.Equals(switchType, NetworkChangePolicy.TypeInternal, StringComparison.OrdinalIgnoreCase);
        var request = new NetworkSwitchChangeRequest(
            options.SwitchName,
            switchType,
            auth,
            AllowManagementOs: allowManagementOs,
            Exists: exists,
            AttachedVmCount: attached);

        return string.Equals(action, NetworkChangePolicy.ActionSwitchRemove, StringComparison.OrdinalIgnoreCase)
            ? NetworkChangePolicy.EvaluateSwitchRemove(request)
            : NetworkChangePolicy.EvaluateSwitchCreate(request);
    }

    private static DesktopNodeHyperVSwitchMutationSnapshot? TryQuery(
        IDesktopNodeHyperVSwitchController switchController,
        string? switchName)
    {
        if (string.IsNullOrWhiteSpace(switchName))
        {
            return null;
        }

        try
        {
            return switchController.Query(switchName);
        }
        catch (DesktopNodeHyperVNativeOperationException)
        {
            return null;
        }
    }

    private static DesktopNodeHostServiceActionResult Success(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeHyperVSwitchMutationSnapshot? snapshot)
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
            ErrorMessage: null,
            HyperVSwitch: snapshot);
    }

    private static DesktopNodeHostServiceActionResult Failure(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeHyperVSwitchMutationSnapshot? snapshot,
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
            HyperVSwitch: snapshot);
    }
}
