namespace DesktopNode.HyperV;

internal static class DesktopNodeHyperVDvdMediaPlan
{
    internal enum Action
    {
        EjectRemoveMedia,
        EjectAlreadyEmpty,
        AttachModifyMedia,
        AttachAddMedia,
    }

    internal static Action Decide(bool mediaPresent, string operation)
    {
        if (string.Equals(operation, "vm.eject", StringComparison.Ordinal))
        {
            return mediaPresent ? Action.EjectRemoveMedia : Action.EjectAlreadyEmpty;
        }

        return mediaPresent ? Action.AttachModifyMedia : Action.AttachAddMedia;
    }
}
