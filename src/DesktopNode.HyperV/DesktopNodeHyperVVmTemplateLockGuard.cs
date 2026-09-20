namespace DesktopNode.HyperV;

public static class DesktopNodeHyperVVmTemplateLockGuard
{
    public const string LockedCode = "PCV_VM_TEMPLATE_LOCKED";

    public static bool IsMutationAllowed(string operation)
    {
        return operation is "vm.start" or "vm.clone" or "vm.create" or "vm.template.lock";
    }

    public static bool TryReject(
        string operation,
        bool templateLock,
        string vmName,
        out DesktopNodeHyperVNativeOperationException? error)
    {
        error = null;
        if (!templateLock || IsMutationAllowed(operation))
        {
            return false;
        }

        error = new DesktopNodeHyperVNativeOperationException(
            LockedCode,
            $"VM '{vmName}' is template-locked.",
            "Template VMs allow start and clone only. Remove template-lock=true to mutate.",
            retryable: false);
        return true;
    }
}
