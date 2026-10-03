namespace DesktopNode.Runtime;

public sealed partial class DesktopNodeJobRuntime
{
    // reconcile 가능한 operation 과 운영자 안내에 쓰는 mutation 이름의 단일 표다. Api 조정 handler 의 dispatch 와 같은 집합이다.
    public static readonly IReadOnlyDictionary<string, string> ReconcilableMutations = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["vm.rename"] = "rename",
        ["vm.delete"] = "delete",
        ["vm.create"] = "create",
        ["vm.shutdown"] = "shutdown",
        ["vm.restart"] = "restart",
        ["vm.start"] = "start",
        ["vm.poweroff"] = "power off",
        ["vm.pause"] = "pause",
        ["vm.resume"] = "resume",
        ["vm.save"] = "save",
        ["vm.resume-saved"] = "resume from saved",
        ["vm.set-memory"] = "resource change",
        ["vm.set-vcpu"] = "resource change",
        ["vm.disk-resize"] = "resource change",
        ["checkpoint.create"] = "checkpoint create",
        ["checkpoint.restore"] = "checkpoint restore",
        ["checkpoint.delete"] = "checkpoint delete",
        ["checkpoint.schedule.set"] = "checkpoint schedule",
        ["checkpoint.schedule.clear"] = "checkpoint schedule clear",
        ["vm.qos.storage.set"] = "storage QoS",
        ["vm.qos.network.set"] = "network QoS",
        ["console.novnc-target.set"] = "noVNC target",
        ["console.novnc-target.clear"] = "noVNC clear",
        ["vm.template.lock"] = "template lock",
        ["vm.network.connect"] = "network connect",
        ["vm.manage"] = "manage",
        ["vm.attach"] = "media attach",
        ["vm.eject"] = "media eject",
    };
}
